# Development stage
FROM node:22-alpine AS dev
WORKDIR /app
EXPOSE 5173
# Command uses host 0.0.0.0 to be accessible from outside the container
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

# Build stage for production
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine AS prod
EXPOSE 80
# Copy built files from the build stage
COPY --from=build /app/dist /usr/share/nginx/html
# Nginx default configuration works well for simple SPA, but we can customize if needed.
# Run Nginx
CMD ["nginx", "-g", "daemon off;"]
