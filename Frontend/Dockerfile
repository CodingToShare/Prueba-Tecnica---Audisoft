# Multi-stage Dockerfile for AngularJS Frontend

# Stage 1: Build dependencies and prepare
FROM node:20-alpine AS builder

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --prefer-offline --no-audit

# Copy all frontend files
COPY . .

# Stage 2: Runtime
FROM node:20-alpine

WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Copy from builder
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app /app

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && adduser -S nodejs -u 1001

# Change ownership to nodejs user
RUN chown -R nodejs:nodejs /app

# Switch to nodejs user
USER nodejs

# Expose frontend port
EXPOSE 8080

# Environment variables defaults
ENV NODE_ENV=production
ENV PORT=8080
ENV API_BASE_URL_PRODUCTION=http://localhost:5281/api/v1
ENV DEBUG_MODE=false

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the server
CMD ["npm", "start"]
