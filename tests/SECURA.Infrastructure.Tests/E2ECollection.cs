namespace SECURA.Infrastructure.Tests;

/// <summary>
/// Forces all E2E test classes into a single sequential collection so that
/// parallel WebApplicationFactory startup doesn't race on Serilog's static logger.
/// </summary>
[CollectionDefinition("E2E", DisableParallelization = true)]
public sealed class E2ECollection { }
