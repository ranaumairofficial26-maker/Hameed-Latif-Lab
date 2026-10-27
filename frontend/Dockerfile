# -----------------------------------------------------------------------------
# Stage 1: Build Angular Application
# -----------------------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /app

# Copy dependency manifests
COPY package*.json ./

# Install dependencies
RUN npm ci --legacy-peer-deps || npm install

# Copy application source code
COPY . .

# Build production bundle
RUN npm run build

# -----------------------------------------------------------------------------
# Stage 2: Serve with Nginx
# -----------------------------------------------------------------------------
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Clean default nginx files
RUN rm -rf ./*

# Copy built artifacts from Stage 1 (Angular outputs to browser/ folder)
COPY --from=build /app/dist/hameed-latif-angular/browser ./

# Copy custom Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose standard HTTP port
EXPOSE 80

# Run nginx in foreground
CMD ["nginx", "-g", "daemon off;"]
