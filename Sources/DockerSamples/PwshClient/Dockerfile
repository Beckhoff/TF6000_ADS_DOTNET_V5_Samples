FROM mcr.microsoft.com/powershell:latest AS final

#Use a local TcXaeMgmt Module for debugging purposes
#COPY TcXaeMgmt/ /TcXaeMgmt

# Install the TcXaeMgmt module
RUN pwsh -Command "Install-Module -Name TcXaeMgmt -Force -Scope AllUsers -AllowClobber -AcceptLicense"

# Copy your script into the container
COPY PwshClient/init.ps1 /scripts/init.ps1

# Run the script and keep the container running
CMD ["pwsh", "-NoExit", "-File", "/scripts/init.ps1"]
