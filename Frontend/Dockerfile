FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY . .
# Create directory for language files
RUN mkdir -p ./Shared/LanguageFiles
RUN rm -f /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 