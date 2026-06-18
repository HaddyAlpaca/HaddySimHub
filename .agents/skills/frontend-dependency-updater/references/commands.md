# Commands & Validation

Run all commands from `ClientApp/`.

## Inventory

```bash
npm outdated
```

## Low-Risk Updates

```bash
npm update
```

## Major Update Examples

```bash
# single package
npm install package-name@latest

# framework family example (Angular core set)
npm install @angular/animations@latest @angular/common@latest @angular/compiler@latest @angular/core@latest @angular/forms@latest @angular/platform-browser@latest @angular/platform-browser-dynamic@latest @angular/router@latest
```

## Validation Commands

```bash
npm run build
npm run test_ci
npm run lint
```

## Optional Security Checks

```bash
npm audit --omit=dev
npm audit fix
```
