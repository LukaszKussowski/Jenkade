# One Jenkins server: Jenkade has a single Connection

Jenkade is a facade for one Jenkins server, in the MVP and after it: the user sets up one Connection, and setting up another replaces it, API token included. Someone who works with several Jenkins servers switches the Connection, or runs a Jenkade per server. This reverses the MVP spec's original stories 4–6 (several Connections, each with a display name, switched between), decided on 2026-10-26.

## Considered options

- **Several Connections with an active one** (the original spec, issue #8): a list, a selected Connection remembered across restarts, data and polling tied to whichever is active. Rejected: Jenkade is meant as the front-end of one Jenkins, not an aggregator of servers.

## Consequences

- The Connection settings form both sets up and replaces the Connection; an empty API token field keeps the saved token, which covers editing (e.g. a new address) without re-entering it.
- Settings hold one Connection (`connection.json`), and the credential store one API token under a fixed key, so replacing a Connection leaves no orphaned token behind.
- The display name stays, as an optional label; without one the Connection is named by its address.
- Removing the Connection (with its token) is still a story of its own; switching between Connections is gone.
