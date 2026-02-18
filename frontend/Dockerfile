# Use a lightweight Node.js 20 image based on Alpine Linux
FROM node:20-alpine

# Set the working directory inside the container
# All following commands run from /app
WORKDIR /app

# Copy package.json and package-lock.json first
# This allows Docker to cache npm install layers
COPY package*.json ./

# Install dependencies
# npm ci = clean, reproducible install (preferred for containers)
RUN npm ci

# Copy the rest of the frontend source code
COPY . .

# Expose Vite’s default dev server port
EXPOSE 5173

# Start the Vite development server
# --host 0.0.0.0 is CRITICAL:
# Without it, Vite only listens inside the container
# and your browser can't access it
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]