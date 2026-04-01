# Best Practices

1. **Always use `provideZonelessChangeDetection()`** in TestBed providers
2. **Use CDK harnesses** for component interaction - keeps tests maintainable
3. **Use `mock<Service>()` from vitest-mock-extended** for type-safe mocks
4. **Use signal-based mocks** for services that use signals
5. **Keep tests focused** - one assertion per test when practical
6. **Use descriptive test names** - `it('should display error when data fails to load')`
7. **Always `await fixture.detectChanges()`** after changing signals/inputs
