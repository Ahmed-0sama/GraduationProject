# Stage 1: Build the .NET app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything to build context
COPY . .

# Build and publish the .NET project (adjust path for space in folder name)
RUN dotnet publish "GraduationProject/Graduation Project.csproj" -c Release -o /app/publish

# Stage 2: Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install Python3, pip, and dependencies needed for Selenium & Chrome
RUN apt-get update && apt-get install -y \
    python3 python3-pip wget unzip gnupg2 curl \
    fonts-liberation libappindicator3-1 libasound2 libatk-bridge2.0-0 libatk1.0-0 libcups2 \
    libdbus-1-3 libdrm2 libgbm1 libgtk-3-0 libnspr4 libnss3 libx11-xcb1 libxcomposite1 \
    libxdamage1 libxrandr2 xdg-utils --no-install-recommends && rm -rf /var/lib/apt/lists/*

# Install Google Chrome stable
RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add - && \
    echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" > /etc/apt/sources.list.d/google-chrome.list && \
    apt-get update && apt-get install -y google-chrome-stable && rm -rf /var/lib/apt/lists/*

# Symlink python3 as py to match your ProcessStartInfo in C# code
RUN ln -s /usr/bin/python3 /usr/bin/py

# Copy and install Python requirements
COPY requirements.txt ./

RUN pip3 install --upgrade pip --break-system-packages
RUN pip3 install --no-cache-dir -r requirements.txt --break-system-packages

# Optional: symlink pip/py/python if needed in C#
ENV PATH="/opt/venv/bin:$PATH"

WORKDIR /app

# Copy the published .NET app files
COPY --from=build /app/publish .

# Copy your Python scripts (adjust path for space)
COPY GraduationProject/webscrapping ./webscrapping

# Set the entry point to your .NET app
ENTRYPOINT ["dotnet", "Graduation Project.dll"]
