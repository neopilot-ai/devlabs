ARG DEBIAN_RELEASE=bookworm
FROM debian:${DEBIAN_RELEASE}

# Install dependencies, add Mono repository, and install Mono and NuGet
RUN apt-get update > /dev/null && \
    apt-get install -y dirmngr ca-certificates gnupg curl && \
    gpg --homedir /tmp --no-default-keyring --keyring /usr/share/keyrings/mono-official-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb [signed-by=/usr/share/keyrings/mono-official-archive-keyring.gpg] https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && \
    apt-get install -y mono-devel && \
    curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe && \
    chmod +x /usr/local/bin/nuget.exe

# Set up an alias for running NuGet with Mono
RUN echo 'alias nuget="mono /usr/local/bin/nuget.exe"' >> /root/.bashrc
