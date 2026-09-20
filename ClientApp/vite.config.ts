import { defineConfig } from 'vite';

export default defineConfig({
  root: 'src',
  optimizeDeps: {
    include: ['chart.js'],
  },
  server: {
    proxy: {
      '/display-data': 'http://localhost:3333',
    },
  },
  build: {
    outDir: '../dist/haddy-sim-hub-client',
    emptyOutDir: true,
  },
});
