# Use an official Node.js runtime as a parent image
FROM node:20-alpine

# Set the working directory in the container
WORKDIR /app

# Copy package.json and package-lock.json to the working directory
COPY package*.json ./

# Install any needed packages
RUN npm install -g @angular/cli
RUN npm install

# Copy the rest of the application's source code from the host to the container's working directory
COPY . .

# Make port 4200 available to the world outside this container
EXPOSE 4200

# Run the app when the container launches
CMD ["ng", "serve", "--host", "0.0.0.0", "--port", "4200"]
