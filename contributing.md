# Contribution Guide

This is a community driven project and contributions are welcome. All contributions are licensed under AGPL 3.0 (the license for this repository).

## Development Prerequisites

### General

- Discord Account and FFXIV installed
- **Your choice of editor** or **VS Code** with C# extensions
- **Git**: For source control
- **(Optional) Github Account**: If you do not wish to use github, you can also submit patch files in the discord.

### Client

- .NET 10. SDK
- FFXIV and Dalamud installed

### Server

- .NET 8.0, 9.0, 10.0 (TODO Migrate all packages to .NET 10)
- **Docker**: For running the server processes
- **Personal Discord Server and Bot**: For installing the bot account

## Running the Server Locally

The server is set up with the docker compose file. To run it you will need a docker compose compatible runtime such as [docker](https://www.docker.com) or [podman](https://podman.io).

1. Ensure that your template.env file internals are copied to a `.env` file with the appropriate details.
2. Rename `config.example.json` to `config.json` file and update desired fields
3. Run the following command from the project root:

```sh
make build-up
```

And can clean up the containers using

```sh
make down
```

## Rules of Thumbs for Contributions

This is a volunteer project maintained in our spare time. Reviewing and maintaining code is timeconsuming, so to reduce the friction and unnecessary code churn, please note that the following will increase the liklihood that we can accept contributions more easily.

1. Coordination takes place in discord, we are happy to give out the contributor role to anyone that wants to help out.
2. Pull Requests should fix or address _one thing_
3. Bug fixes are greatly appreciated
4. Large features, should be discussed in the discord prior to engaging in and working on it.
5. PRs that are an optimization must provide some testable metrics/data for the performance improvement (e.g. database query time, end to end latency, etc)

### A note on AI contributions

There is no specific issue with the use of Agentic AI for development purposes; however, we consider the contributor to have been the writer and this includes and low quality code generated from the LLM.

AI is a tool to augment, not replace a contributor's problem solving skills.

## Process

If you would like to contribute, please create a fork of the repo and make your changes on a separate branch and create a pull request.

Please make the name of the branch vaguely match what is being developed. (This is more maintainer sanity)

Additionally, to reduce friction when submitting contributions, please consider the following guidelines.

### Commit Message Guidelines

Try to be descriptive of what was changed and why
[Conventional Commits](https://www.conventionalcommits.org/) spec is a good general guideline.

The following is generally a good rule of thumb:

```
<type>(scope): <Description of change>

[Body if change is big enough]
```

#### Common commit types**

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `chore`: Maintenance tasks (bumping dependencies, fixing ui strings, etc)
- `perf`: Performance optimizations

Mainly try to provide a reasonable description of what the commit entails so it's relatively easy to look back on what happened.

#### Example

##### Good commit

```
fix (client): Fixed a bug related to search queries not handling special characters
```

```
feat (client/server): Added a collar slot to the UI
Added tab to wardrobe UI along with permissions
Updated Server services to include the checks and functionality for specific perms
Updated schema to include special permissions for this slot
```

##### Bad Commits Messages

```
idk: I fixed a bug in the UI
```

```
whatever: i told claud code to add a collar slot
```

### Before you create a Pull Requests

1. Run `make fmt` to ensure that the project is formatted according to the C# style guide.

2. Rebase your commit onto main (if possible)

   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

3. On your repository, create a pull request from your feature branch:
   - Clear title following commit message format
   - Any additional details not in the commit history.
   - Testing instructions if applicable
   - Related issue numbers if applicable
   - Screenshots of UI changes if applicable

## Licensing

All contributions are licensed under the AGPL 3.0 license.
