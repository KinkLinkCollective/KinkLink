# Contribution Guide

This is a community driven project and contributions are welcome. All contributions are licensed under AGPL 3.0 (the license for this repository).

## Running the Server Locally

The server is set up with the docker compose file. To run it you will need a docker compose compatible runtime such as [docker](https://www.docker.com) or [podman](https://podman.io).

1. Ensure that your template.env file internals are copied to a `.env` file with the appropriate details.
2. Customize the config.json file if necessary.
3. Run the following command from the project root:

```sh
docker compose up -d --build
```

And can clean up the containers using

```sh
docker compose down
```

## Process

If you would like to contribute, please create a fork of the repo and make your changes on a separate branch and create a pull request.

Please ensure that you provide a full description of the changes and how they influence the plugin.

## Contribution Rules of Thumbs

Smaller contributions such as bugfixes or non-distruptive feature enhancements are welcome, but for larger features, please join the discord and discuss with our team about the scope of your proposed changes before you start working.
