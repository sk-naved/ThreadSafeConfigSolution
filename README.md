# Thread-Safe Configuration Service (.NET)

A production-ready implementation of a thread-safe configuration provider designed for high-concurrency financial systems.

## Key Architectural Features
* **Double-Checked Locking:** Ensures the database/source is only hit once even when multiple threads request data simultaneously.
* **Non-Blocking Synchronization:** Utilizes `SemaphoreSlim` to prevent thread pool starvation in asynchronous workflows.
* **Fast-Path Optimization:** Prioritizes in-memory retrieval to ensure sub-millisecond response times for valid cache entries.

## Why this approach?
In enterprise-grade applications (like those at Morgan Stanley or Scania), standard `lock` statements can lead to performance bottlenecks. This implementation focuses on scalability and resource efficiency.

## Tech Stack
* .NET 8.0+
* C# (Async/Await)
* SemaphoreSlim for Concurrency Management
