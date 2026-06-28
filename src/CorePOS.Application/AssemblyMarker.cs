namespace CorePOS.Application;

/// <summary>
/// Marker class used to reference this assembly for MediatR registration.
/// Used in: services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly))
/// </summary>
public sealed class AssemblyMarker { }
