# Stage 1: build
FROM node:22-alpine AS build
WORKDIR /app
COPY src/AdminDashboard.React/package.json ./
RUN npm install
COPY src/AdminDashboard.React/ .
RUN npm run build

# Stage 2: serve with nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY src/AdminDashboard.React/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
