# Orleans Distributed Demo

This project demonstrates a distributed application using .NET 9 and Orleans. It consists of a simple distributed counter that can be accessed from multiple clients across different silo instances, showcasing the distributed nature of Orleans.

## Project Structure

The solution is organized into the following projects:

- `Interfaces`: Contains the grain interfaces
- `Server`: The silo host application
- `Client`: The client application that connects to the Orleans cluster

## Prerequisites

- .NET 9 SDK or later

## Building the Solution

```bash
# Build the solution
dotnet build
```

## Running the Demo

### 1. Start the Primary Silo

Start the primary silo with default configuration:

```bash
dotnet run --project Server/Server.csproj
```

This will start a silo named "Silo1" with default ports:
- Silo port: 11111
- Gateway port: 30000

### 2. Start Additional Silos (Optional, for demonstrating distribution)

Start a second silo with a different name and ports (in a new terminal):

```bash
dotnet run --project Server/Server.csproj Silo2 11112 30001
```

Start a third silo (in a new terminal):

```bash
dotnet run --project Server/Server.csproj Silo3 11113 30002
```

### 3. Start the Client

Start the client connecting to the primary silo (in a new terminal):

```bash
dotnet run --project Client/Client.csproj
```

To connect to a different silo, specify the gateway port:

```bash
dotnet run --project Client/Client.csproj 30001
```


## Verifying Distributed Execution

1. Start multiple silos as described above
2. Start multiple clients connected to different silos
3. Use the same grain key in all clients
4. Perform operations in one client and observe that the changes are reflected in other clients
5. Use the "Get host information" option to see which silo is hosting the grain - you'll notice the grain can move between silos

## Features Demonstrated

- Distributed state management
- Transparent activation and deactivation of grains
- Location transparency
- Fault tolerance (try shutting down a silo and notice the grain continues to function)
- Concurrency handling (try operating on the same grain from multiple clients simultaneously)

## Architecture

This application demonstrates the virtual actor model provided by Orleans, where:

1. Grains are the unit of state and computation
2. Silos host and manage the lifecycle of grains
3. Clients interact with grains through their interfaces
4. The Orleans runtime handles grain placement, activation, deactivation, and communication

## Monitoring

Each silo hosts a dashboard on port `siloPort + 1000` (e.g., http://localhost:12111 for the primary silo).
The dashboard provides real-time information about the cluster, silos, and grain activations.
