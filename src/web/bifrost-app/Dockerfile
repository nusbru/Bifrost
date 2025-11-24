# syntax=docker/dockerfile:1

# ============================================================================
# Base Stage - Common dependencies for all stages
# ============================================================================
FROM node:22-alpine AS base

# Install security updates and libc6-compat for Alpine compatibility
# See: https://github.com/nodejs/docker-node/tree/main#nodealpine
RUN apk add --no-cache libc6-compat

WORKDIR /app

# ============================================================================
# Dependencies Stage - Install production dependencies only
# ============================================================================
FROM base AS deps

# Copy package manager lock files
COPY package.json package-lock.json* ./

# Install dependencies based on package manager
# Use npm ci for reproducible builds with exact versions from package-lock.json
RUN \
  if [ -f package-lock.json ]; then npm ci; \
  else echo "Lockfile not found." && exit 1; \
  fi

# ============================================================================
# Builder Stage - Build the Next.js application
# ============================================================================
FROM base AS builder

WORKDIR /app

# Copy dependencies from deps stage
COPY --from=deps /app/node_modules ./node_modules

# Copy application source
COPY . .

# Declare build-time environment variables
# Next.js will inline NEXT_PUBLIC_* variables into the JavaScript bundle
# These values are NOT secrets - they are sent to the browser
ARG NEXT_PUBLIC_SUPABASE_URL
ARG NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY

# Set environment variables for Next.js build
# These are replaced at build time in the client-side bundle
ENV NEXT_PUBLIC_SUPABASE_URL=${NEXT_PUBLIC_SUPABASE_URL}
ENV NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY=${NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY}
ENV NEXT_TELEMETRY_DISABLED=1

# Build Next.js application
# This creates .next/standalone with minimal dependencies
RUN npm run build

# ============================================================================
# Runner Stage - Minimal production image
# ============================================================================
FROM base AS runner

WORKDIR /app

# Set production environment
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# Create non-root user for security best practices
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

# Copy standalone output - automatically traced minimal dependencies
# See: https://nextjs.org/docs/app/api-reference/config/next-config-js/output
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

# Note: public folder not copied as it doesn't exist in this project
# If you add public assets later, uncomment:
# COPY --from=builder --chown=nextjs:nodejs /app/public ./public

# Switch to non-root user
USER nextjs

# Expose application port
EXPOSE 3000

# Configure server hostname and port
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Server-side environment variables (NOT exposed to browser)
# These can be changed at runtime without rebuilding
# API_URL: Internal URL for server-side API calls to .NET backend
# SUPABASE_URL: Server-side Supabase URL (can be same as NEXT_PUBLIC_ version)
# SUPABASE_SERVICE_ROLE_KEY: Admin key for server-side Supabase operations (SECRET)

# Health check for container orchestration
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD node -e "require('http').get('http://localhost:3000/api/health', (r) => {process.exit(r.statusCode === 200 ? 0 : 1)})" || exit 1

# Start Next.js standalone server
CMD ["node", "server.js"]
