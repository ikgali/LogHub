namespace LogHub.Processing;

public sealed record ProcessingResult(int Read, int Written, int Skipped);
