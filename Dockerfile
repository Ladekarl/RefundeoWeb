FROM microsoft/aspnetcore-build:2.0.2
ENV ASPNETCORE_ENVIRONMENT Development
COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
EXPOSE 80/tcp
RUN apt-get update && apt-get install -y \
    libgdiplus
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh
