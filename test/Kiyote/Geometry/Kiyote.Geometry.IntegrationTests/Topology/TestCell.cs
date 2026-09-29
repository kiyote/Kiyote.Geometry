namespace Kiyote.Geometry.Topology.IntegrationTests;

public record struct TestCell(
	bool IsWalkable,
	bool IsGasPermeable,
	bool IsVacuum
);
