import {
  sanitizeInput,
  isValidEmail,
  isValidPassword,
  isValidUsername,
} from "./utils.js";
import { apiRequest } from "./api.js";

const API_BASE_URL = "/api";

const GOOGLE_CLIENT_ID =
  "157841978934-fmgq60lshk9iq65s7h37mc7ta78m8nu3.apps.googleusercontent.com";
const GOOGLE_REDIRECT_URI = window.location.origin + "/login";

function getGoogleOAuthUrl() {
  const params = new URLSearchParams({
    client_id: GOOGLE_CLIENT_ID,
    redirect_uri: GOOGLE_REDIRECT_URI,
    response_type: "code",
    scope: "openid email profile",
    access_type: "offline",
    prompt: "consent",
  });
  return `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`;
}

document
  .getElementById("google-login-btn")
  ?.addEventListener("click", function () {
    window.location.href = getGoogleOAuthUrl();
  });

window.addEventListener("DOMContentLoaded", async function () {
  const urlParams = new URLSearchParams(window.location.search);
  const code = urlParams.get("code");
  if (code) {
    try {
      const language = getCurrentLanguage();
      const res = await fetch(`${API_BASE_URL}/Auth/login/google`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          code: code,
          redirectUri: GOOGLE_REDIRECT_URI,
          language,
        }),
      });
      const data = await res.json();
      const errorCode = data.errorCode || data.ErrorCode;
      if (res.ok && data.token) {
        localStorage.setItem("authToken", data.token);
        if (data.language) {
          window.i18next.changeLanguage(data.language);
        }
        safeShowToastrMessage(
          window.i18next.t("googleLoginSuccessfulRedirecting"),
          "success",
        );
        setTimeout(() => {
          window.location.href = "/admin/";
        }, 1000);
      } else {
        const errorCode = data.errorCode || data.ErrorCode;
        const errorMessage = data.message || window.i18next.t("googleLoginFailed");
        if (errorCode === "ACCOUNT_NOT_VERIFIED" || errorMessage === "accountNotVerified") {
          safeShowToastrMessage(window.i18next.t("accountNotVerified"), "error");
        } else if (
          errorCode === "ACCOUNT_DELETED" ||
          errorMessage.includes("deleted")
        ) {
          safeShowToastrMessage(
            window.i18next.t("accountHasBeenDeletedContactSupport"),
            "error",
          );
        } else if (
          errorCode === "ACCOUNT_BANNED" ||
          errorMessage.includes("banned")
        ) {
          safeShowToastrMessage(
            window.i18next.t("yourAccountHasBeenDeactivated"),
            "error",
          );
        } else {
          safeShowToastrMessage(window.i18next.t(errorMessage), "error");
        }
      }
    } catch (err) {
      safeShowToastrMessage(
        window.i18next.t("googleLoginFailedPleaseTryAgain"),
        "error",
      );
    }
  }
});

window.addEventListener("DOMContentLoaded", async function () {
  const emailElem = document.getElementById("reset-password-email");
  const descElem = document.querySelector(
    '[data-i18n="auth.reset-password.description"]',
  );
  if (emailElem) {
    let email = "";
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get("token");
    if (token) {
      try {
        const res = await fetch(
          `${API_BASE_URL}/Auth/validate-reset-token?token=${encodeURIComponent(token)}`,
        );
        const data = await res.json();
        if (res.ok && data.success && data.email) {
          email = data.email;
        } else {
          safeShowToastrMessage(
            data.message || window.i18next.t("invalidOrExpiredResetToken"),
            "error",
          );
        }
      } catch (err) {
        safeShowToastrMessage(
          window.i18next.t("failedToValidateResetToken"),
          "error",
        );
      }
    }
    emailElem.textContent = email || "not available";
    if (descElem) {
      descElem.textContent = window.i18next
        .t("auth.reset-password.description")
        .replace("{email}", email || "...");
    }
  }
});

window.addEventListener("DOMContentLoaded", function () {
  const userEmailElem = document.getElementById("userEmail");
  if (userEmailElem) {
    userEmailElem.textContent = window.i18next
      ? window.i18next.t("notAvailable")
      : "not available";
  }
});

if (document.getElementById("login-form")) {
  const loginForm = document.getElementById("login-form");
  let otpStep = 1;
  let lastLoginPayload = null;
  let otpModal = null;
  let otpInputElem = null;
  function updateOtpModalI18n() {
    const modal = document.getElementById("otpModal");
    if (!modal) return;
    $("#otpModalLabel").text(window.i18next.t("twoFactorAuth"));
    $("#otpModalInput").attr("placeholder", window.i18next.t("enterOtpCode"));
    $("#otpModal .modal-body .mb-2").text(window.i18next.t("enterOtpCode"));
    $("#otpModalSubmit").text(window.i18next.t("verifyOtp"));
    $("#otpModalError").text("");
    $("#otpModal .btn-close").attr("aria-label", window.i18next.t("close"));
  }
  if (window.i18next) {
    window.i18next.on("languageChanged", function () {
      updateOtpModalI18n();
    });
  }
  function ensureOtpModal() {
    if (document.getElementById("otpModal")) return;
    const modalHtml = `
      <div class="modal fade" id="otpModal" tabindex="-1" aria-labelledby="otpModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-sm modal-dialog-centered">
          <div class="modal-content p-3">
            <div class="modal-header py-2">
              <h5 class="modal-title" id="otpModalLabel">${window.i18next.t("twoFactorAuth")}</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="${window.i18next.t("close")}"></button>
            </div>
            <div class="modal-body pt-2 pb-1">
              <div class="mb-2 small">${window.i18next.t("enterOtpCode")}</div>
              <input type="text" class="form-control form-control-sm mb-2 text-center" id="otpModalInput" maxlength="8" autocomplete="one-time-code" placeholder="${window.i18next.t("enterOtpCode")}" style="font-size:1.1em;letter-spacing:2px;" />
              <div class="text-danger small" id="otpModalError"></div>
            </div>
            <div class="modal-footer pt-1 pb-2 border-0">
              <button type="button" class="btn btn-primary btn-sm w-100" id="otpModalSubmit">${window.i18next.t("verifyOtp")}</button>
            </div>
          </div>
        </div>
      </div>`;
    $("body").append(modalHtml);
    otpModal = new bootstrap.Modal(document.getElementById("otpModal"));
    updateOtpModalI18n();
  }
  loginForm.addEventListener("submit", async function (e) {
    e.preventDefault();
    const email = sanitizeInput(document.getElementById("login-email").value);
    const password = document.getElementById("login-password").value;
    const errors = [];
    if (!email) {
      errors.push(window.i18next.t("emailRequired"));
    } else if (!isValidEmail(email)) {
      errors.push(window.i18next.t("pleaseEnterValidEmailAddress"));
    }
    if (!password) {
      errors.push(window.i18next.t("passwordRequired"));
    } else if (password.length < 6) {
      errors.push(window.i18next.t("passwordMustBeAtLeast6Characters"));
    }
    if (errors.length > 0) {
      safeShowToastrMessage(errors.join("\n"), "error");
      return;
    }
    try {
      const language = getCurrentLanguage();
      let payload = { email, password, language };
      if (otpStep === 2 && lastLoginPayload && otpInputElem) {
        payload = { ...lastLoginPayload, otpCode: otpInputElem.value.trim() };
      }
      const res = await fetch(`${API_BASE_URL}/Auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      const data = await res.json();
      if (res.ok && data.token) {
        localStorage.setItem("authToken", data.token);
        if (data.language) {
          window.i18next.changeLanguage(data.language);
        }
        safeShowToastrMessage(window.i18next.t("loginSuccessfulRedirecting"), "success");
        setTimeout(() => {
          window.location.href = "/admin/";
        }, 1000);
      } else if (data.require2FA) {
        otpStep = 2;
        lastLoginPayload = { email, password, language };
        ensureOtpModal();
        otpInputElem = document.getElementById("otpModalInput");
        document.getElementById("otpModalError").textContent = "";
        otpInputElem.value = "";
        otpInputElem.focus();
        otpModal = otpModal || new bootstrap.Modal(document.getElementById("otpModal"));
        otpModal.show();
        safeShowToastrMessage(window.i18next.t("pleaseEnter2faCode"), "info");
        $("#otpModalSubmit").off("click").on("click", async function () {
          const otpVal = otpInputElem.value.trim();
          if (!otpVal) {
            document.getElementById("otpModalError").textContent = window.i18next.t("otpRequired");
            return;
          }
          const otpPayload = { ...lastLoginPayload, otpCode: otpVal };
          const res2 = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(otpPayload),
          });
          const data2 = await res2.json();
          if (res2.ok && data2.token) {
            localStorage.setItem("authToken", data2.token);
            if (data2.language) {
              window.i18next.changeLanguage(data2.language);
            }
            otpModal.hide();
            safeShowToastrMessage(window.i18next.t("loginSuccessfulRedirecting"), "success");
            setTimeout(() => {
              window.location.href = "/admin/";
            }, 1000);
          } else {
            document.getElementById("otpModalError").textContent = window.i18next.t("invalidOtpCode");
          }
        });
        $("#otpModalInput").off("keydown").on("keydown", function (e) {
          if (e.key === "Enter") {
            $("#otpModalSubmit").click();
          }
        });
        return;
      } else {
        if (window.errorHandler && data) {
          window.errorHandler.handleApiError(data);
        }
        if (data.errors && Array.isArray(data.errors)) {
          safeShowToastrMessage(
            data.errors.map((e) => window.i18next.t(e)).join(", "),
            "error",
          );
        } else {
          const errorCode = data.errorCode || data.ErrorCode;
          const errorMessage = data.message || "loginFailed";
          if (
            errorCode === "ACCOUNT_NOT_VERIFIED" || errorMessage === "accountNotVerified"
          ) {
            safeShowToastrMessage(window.i18next.t("accountNotVerified"), "error");
          } else if (errorMessage.includes("deleted")) {
            safeShowToastrMessage(
              window.i18next.t("accountHasBeenDeletedContactSupport"),
              "error",
            );
          } else if (errorMessage.includes("banned")) {
            safeShowToastrMessage(
              window.i18next.t("yourAccountHasBeenDeactivated"),
              "error",
            );
          } else if (
            errorMessage.includes("Invalid email or password") ||
            errorMessage.includes("Email hoặc mật khẩu không đúng") ||
            errorMessage.includes("メールアドレスまたはパスワードが正しくありません")
          ) {
            safeShowToastrMessage(window.i18next.t("invalidCredentials"), "error");
          } else {
            safeShowToastrMessage(window.i18next.t(errorMessage), "error");
          }
        }
      }
    } catch (err) {
      safeShowToastrMessage(window.i18next.t("loginFailedPleaseTryAgain"), "error");
    }
  });
}

function showUsernameSuggestionModal(original, suggested, onAccept, onReject) {
  const msg = window.i18next.t("usernameSuggestionMessage").replace("{original}", original).replace("{suggested}", suggested);
  const modalHtml = `
    <div class="modal fade" id="usernameSuggestionModal" tabindex="-1" aria-labelledby="usernameSuggestionModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="usernameSuggestionModalLabel">${window.i18next.t("usernameSuggestionTitle")}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">${msg}</div>
          <div class="modal-footer">
            <button type="button" class="btn btn-primary" id="acceptSuggestedUsernameBtn">${window.i18next.t("yes")}</button>
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${window.i18next.t("no")}</button>
          </div>
        </div>
      </div>
    </div>`;
  $("body").append(modalHtml);
  const modal = new bootstrap.Modal(document.getElementById("usernameSuggestionModal"));
  modal.show();
  $("#acceptSuggestedUsernameBtn").on("click", function () {
    modal.hide();
    $("#usernameSuggestionModal").remove();
    onAccept();
  });
  $("#usernameSuggestionModal").on("hidden.bs.modal", function () {
    $("#usernameSuggestionModal").remove();
    if (onReject) onReject();
  });
}

if (document.getElementById("register-form")) {
  const registerForm = document.getElementById("register-form");
  registerForm.addEventListener("submit", async function (e) {
    e.preventDefault();

    const username = sanitizeInput(
      document.getElementById("register-username").value,
    );
    const fullName = sanitizeInput(
      document.getElementById("register-fullName").value,
    );
    const email = sanitizeInput(
      document.getElementById("register-email").value,
    );
    const phoneNumber = sanitizeInput(
      document.getElementById("register-phoneNumber")?.value,
    );
    const password = document.getElementById("register-password").value;
    const termsChecked = document.getElementById("terms-conditions")?.checked;
    const language = getCurrentLanguage();

    const errors = [];

    if (!username) {
      errors.push(window.i18next.t("usernameRequired"));
    } else if (!isValidUsername(username)) {
      errors.push(window.i18next.t("usernameInvalid"));
    }

    if (!email) {
      errors.push(window.i18next.t("emailRequired"));
    } else if (!isValidEmail(email)) {
      errors.push(window.i18next.t("pleaseEnterValidEmailAddress"));
    }

    if (!password) {
      errors.push(window.i18next.t("passwordRequired"));
    } else if (!isValidPassword(password)) {
      errors.push(window.i18next.t("passwordInvalid"));
    }

    if (fullName && !/^[a-zA-ZÀ-ỹ\s]+$/.test(fullName)) {
      errors.push(window.i18next.t("fullNameInvalidCharacters"));
    }

    if (phoneNumber && !/^[0-9]{10,11}$/.test(phoneNumber.replace(/\D/g, ""))) {
      errors.push(window.i18next.t("phoneNumberInvalidFormat"));
    }

    if (!termsChecked) {
      errors.push(window.i18next.t("mustAgreeToTerms"));
    }

    if (errors.length > 0) {
      safeShowToastrMessage(errors.filter(Boolean).join(", "), "error");
      return;
    }

    async function submitRegister({ username, fullName, email, phoneNumber, password, language, acceptSuggestedUsername }) {
      const res = await fetch(`${API_BASE_URL}/Auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          username,
          fullName: fullName || null,
          email,
          phoneNumber: phoneNumber || null,
          password,
          language,
          acceptSuggestedUsername: !!acceptSuggestedUsername,
        }),
      });
      return res.json().then(data => ({ data, ok: res.ok }));
    }

    try {
      let registerResult = await submitRegister({ username, fullName, email, phoneNumber, password, language });
      let { data, ok } = registerResult;
      if (ok) {
        if (data.username && data.username !== username) {
          safeShowToastrMessage(
            window.i18next
              .t("usernameAutoGenerated")
              .replace("{original}", username)
              .replace("{generated}", data.username),
            "error",
          );
        } else {
          safeShowToastrMessage(
            window.i18next.t("registrationSuccessfulCheckEmail"),
            "success",
          );
        }
        setTimeout(() => {
          window.location.href = "/auth/verify-email.html";
        }, 1000);
      } else if (data.suggestedUsername) {
        showUsernameSuggestionModal(username, data.suggestedUsername, async () => {
          let retryResult = await submitRegister({
            username: data.suggestedUsername,
            fullName,
            email,
            phoneNumber,
            password,
            language,
            acceptSuggestedUsername: true,
          });
          let { data: retryData, ok: retryOk } = retryResult;
          if (retryOk) {
            safeShowToastrMessage(
              window.i18next.t("registrationSuccessfulCheckEmail"),
              "success",
            );
            setTimeout(() => {
              window.location.href = "/auth/verify-email.html";
            }, 1000);
          } else {
            safeShowToastrMessage(window.i18next.t(retryData.message || "registrationFailed"), "error");
          }
        }, () => {
          $("#register-username").focus();
        });
      } else {
        if (data.errors && Array.isArray(data.errors)) {
          safeShowToastrMessage(
            data.errors.map((e) => window.i18next.t(e)).join(", "),
            "error",
          );
        } else {
          const errorMessage = data.message || "registrationFailed";
          if (errorMessage.includes("already exists")) {
            if (errorMessage.includes("Username")) {
              safeShowToastrMessage(
                window.i18next
                  .t("usernameAlreadyExists")
                  .replace("{username}", username),
                "error",
              );
            } else {
              safeShowToastrMessage(
                window.i18next.t("userAlreadyExists").replace("{email}", email),
                "error",
              );
            }
          } else {
            safeShowToastrMessage(window.i18next.t(errorMessage), "error");
          }
        }
      }
    } catch (err) {
      safeShowToastrMessage(
        window.i18next.t("registrationFailedTryAgain"),
        "error",
      );
    }
  });
}

if (document.getElementById("forgot-password-form")) {
  const forgotPasswordForm = document.getElementById("forgot-password-form");
  forgotPasswordForm.addEventListener("submit", async function (e) {
    e.preventDefault();

    const email = sanitizeInput(
      document.getElementById("forgot-password-email").value,
    );
    const language = getCurrentLanguage();

    if (!email) {
      safeShowToastrMessage(window.i18next.t("emailRequired"), "error");
      return;
    }

    if (!isValidEmail(email)) {
      safeShowToastrMessage(
        window.i18next.t("pleaseEnterValidEmailAddress"),
        "error",
      );
      return;
    }

    try {
      const res = await fetch(`${API_BASE_URL}/Auth/forgot-password`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, language }),
      });
      const data = await res.json();

      if (res.ok) {
        localStorage.setItem("pendingResetEmail", email);
        safeShowToastrMessage(
          window.i18next.t("resetEmailSentCheckEmail"),
          "success",
        );
        setTimeout(() => {
          window.location.href = "/auth/login.html";
        }, 1500);
        forgotPasswordForm.reset();
      } else {
        safeShowToastrMessage(
          window.i18next.t(data.message || "failedToSendResetEmail"),
          "error",
        );
      }
    } catch (err) {
      safeShowToastrMessage(
        window.i18next.t("failedToSendResetEmailTryAgain"),
        "error",
      );
    }
  });
}

if (document.getElementById("reset-password-form")) {
  const resetPasswordForm = document.getElementById("reset-password-form");
  resetPasswordForm.addEventListener("submit", async function (e) {
    e.preventDefault();

    const password = document.getElementById("reset-password-password").value;
    const confirmPassword = document.getElementById(
      "reset-password-confirm-password",
    ).value;
    const language = getCurrentLanguage();

    if (!password) {
      safeShowToastrMessage(window.i18next.t("passwordRequired"), "error");
      return;
    }

    if (!isValidPassword(password)) {
      safeShowToastrMessage(window.i18next.t("passwordInvalid"), "error");
      return;
    }

    if (password !== confirmPassword) {
      safeShowToastrMessage(window.i18next.t("passwordsDoNotMatch"), "error");
      return;
    }

    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get("token");

    if (!token) {
      safeShowToastrMessage(window.i18next.t("invalidResetToken"), "error");
      return;
    }

    try {
      const res = await fetch(`${API_BASE_URL}/Auth/reset-password`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          token: token,
          newPassword: password,
          confirmPassword: confirmPassword,
          language,
        }),
      });
      const data = await res.json();

      if (res.ok) {
        safeShowToastrMessage(
          window.i18next.t("passwordResetSuccessRedirectLogin"),
          "success",
        );
        localStorage.removeItem("pendingResetEmail");
        setTimeout(() => {
          window.location.href = "/auth/login.html";
        }, 2000);
      } else {
        safeShowToastrMessage(
          window.i18next.t(data.message || "passwordResetFailed"),
          "error",
        );
      }
    } catch (err) {
      safeShowToastrMessage(
        window.i18next.t("passwordResetFailedTryAgain"),
        "error",
      );
    }
  });
}

if (document.getElementById("change-password-form")) {
  const changePasswordForm = document.getElementById("change-password-form");
  changePasswordForm.addEventListener("submit", async function (e) {
    e.preventDefault();

    const currentPassword = document.getElementById(
      "change-password-current",
    ).value;
    const newPassword = document.getElementById("change-password-new").value;
    const confirmPassword = document.getElementById(
      "change-password-confirm",
    ).value;
    const language = getCurrentLanguage();

    if (!currentPassword) {
      safeShowToastrMessage(window.i18next.t("currentPasswordRequired"), "error");
      return;
    }

    if (!newPassword) {
      safeShowToastrMessage(window.i18next.t("newPasswordRequired"), "error");
      return;
    }

    if (!isValidPassword(newPassword)) {
      safeShowToastrMessage(window.i18next.t("newPasswordInvalid"), "error");
      return;
    }

    if (newPassword !== confirmPassword) {
      safeShowToastrMessage(window.i18next.t("newPasswordsDoNotMatch"), "error");
      return;
    }

    const token = localStorage.getItem("authToken");
    if (!token) {
      safeShowToastrMessage(
        window.i18next.t("mustBeLoggedInToChangePassword"),
        "error",
      );
      return;
    }

    try {
      const res = await fetch(`${API_BASE_URL}/Auth/change-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          currentPassword,
          newPassword,
          confirmPassword,
          language,
        }),
      });
      const data = await res.json();

      if (res.ok) {
        safeShowToastrMessage(
          window.i18next.t("passwordChangedSuccessfully"),
          "success",
        );
        changePasswordForm.reset();
      } else {
        safeShowToastrMessage(
          window.i18next.t(data.message || "passwordChangeFailed"),
          "error",
        );
      }
    } catch (err) {
      safeShowToastrMessage(
        window.i18next.t("passwordChangeFailedTryAgain"),
        "error",
      );
    }
  });
}

window.addEventListener("DOMContentLoaded", function () {
  if (
    !window.location.pathname.endsWith("verify-email.html") &&
    !window.location.pathname.endsWith("verify-email")
  )
    return;
  const verifySuccessSection = document.getElementById(
    "verify-success-section",
  );
  if (verifySuccessSection) verifySuccessSection.style.display = "block";
});
window.addEventListener("DOMContentLoaded", async function () {
  if (
    !window.location.pathname.endsWith("account-activated.html") &&
    !window.location.pathname.endsWith("account-activated")
  )
    return;
  const urlParams = new URLSearchParams(window.location.search);
  const token = urlParams.get("token");
  const countdownElem = document.getElementById("countdown");
  let countdown = 5;
  function startCountdown() {
    if (countdownElem) countdownElem.textContent = countdown;
    const interval = setInterval(() => {
      countdown--;
      if (countdownElem) countdownElem.textContent = countdown;
      if (countdown <= 0) {
        clearInterval(interval);
        window.location.href = "/auth/login.html";
      }
    }, 1000);
  }
  if (!token) {
    safeShowToastrMessage(window.i18next.t("invalidOrExpiredToken"), "error");
    if (countdownElem) countdownElem.textContent = "-";
    return;
  }
  try {
    const res = await fetch(
      `${API_BASE_URL}/Auth/verify-email?token=${encodeURIComponent(token)}`,
    );
    const data = await res.json();
    if (res.ok && data.success) {
      safeShowToastrMessage(
        window.i18next.t("emailVerifiedSuccessfully"),
        "success",
      );
      startCountdown();
    } else {
      let msg = data.message || "invalidOrExpiredToken";
      safeShowToastrMessage(window.i18next.t(msg), "error");
      if (countdownElem) countdownElem.textContent = "-";
    }
  } catch (err) {
    safeShowToastrMessage(window.i18next.t("verifyEmailFailed"), "error");
    if (countdownElem) countdownElem.textContent = "-";
  }
});

function getCurrentLanguage() {
  return window.i18next?.language || localStorage.getItem("i18nextLng") || "en";
}

function safeShowToastr(msg, type = "success") {
  if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
    msg = window.i18next.t(msg);
  }
  window.showToastr(msg, type);
}

function safeShowToastrMessage(msg, type = "success") {
  if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
    if (typeof msg === "string" && (!msg.trim().includes(" ") || msg === msg.toUpperCase())) {
      msg = window.i18next.t(msg);
    }
  }
  safeShowToastr(msg, type);
}
