// Injector and ControllerManager rely on the global static Injector registry,
// so tests must not run in parallel across classes.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
