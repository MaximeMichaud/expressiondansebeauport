import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import svgLoader from 'vite-svg-loader'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [
    vue(),
    svgLoader(),
    tailwindcss()
  ],
  base: '/',
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    chunkSizeWarningLimit: 1000,
    rolldownOptions: {
      output: {
        entryFileNames: 'js/[name]-[hash].js',
        chunkFileNames: 'js/[name]-[hash].js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name?.endsWith('.css')) {
            return 'css/[name]-[hash][extname]'
          }
          return 'assets/[name]-[hash][extname]'
        }
      }
    }
  },
  css: {
    lightningcss: {
      errorRecovery: true
    }
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 8080,
    proxy: {
      '/api': {
        target: 'http://localhost:5280',
        changeOrigin: true,
        secure: false
      },
      '/uploads': {
        target: 'http://localhost:5280',
        changeOrigin: true,
        secure: false
      },
      '/hubs': {
        target: 'http://localhost:5280',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  }
})
