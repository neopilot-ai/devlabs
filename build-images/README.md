# build-images

This project publishes editor-extensions related container images for re-use across
projects to the NeoPilot Container Registry.

## `node-codesign`

Extends the [node](https://hub.docker.com/_/node) image with some additions:

- [Rust and Cargo](https://doc.rust-lang.org/cargo/getting-started/installation.html)
- [apple-codesign](https://crates.io/crates/apple-codesign)

## `mono-nuget`

Extends the [debian:bookworm](https://hub.docker.com/_/debian) image with:

- [NuGet and Mono](https://learn.microsoft.com/en-us/nuget/install-nuget-client-tools?tabs=macos#install-nugetexe)

## `net-framework-vs-workload`

Extends the [dotnet-framework-docker](https://github.com/microsoft/dotnet-framework-docker/blob/main/README.sdk.md) image with:

- [Visual Studio workloads](https://learn.microsoft.com/en-us/visualstudio/install/use-command-line-parameters-to-install-visual-studio?view=vs-2022)

