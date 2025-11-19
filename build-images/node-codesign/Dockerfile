ARG NODE_IMAGE_VERSION=18
FROM node:${NODE_IMAGE_VERSION}

RUN apt update -qy && \
  apt install -qy zip && \
  curl https://sh.rustup.rs -sSf | sh -s -- -y
ENV PATH="/root/.cargo/bin:${PATH}"
RUN cargo install --git https://github.com/indygreg/apple-platform-rs --branch main --bin rcodesign apple-codesign
ADD ./codesign /usr/local/bin/codesign
ADD ./macos-entitlements.xml /opt/neopilot-codesigning/macos-entitlements.xml
