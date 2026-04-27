import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import svgLoader from 'vite-svg-loader'
import tailwindcss from '@tailwindcss/vite'
import { compression } from 'vite-plugin-compression2'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [
    vue(),
    svgLoader(),
    tailwindcss(),
    compression({ algorithm: 'brotliCompress', exclude: [/\.(png|jpg|jpeg|gif|webp|ico|svg)$/] }),
    compression({ algorithm: 'gzip', exclude: [/\.(png|jpg|jpeg|gif|webp|ico|svg)$/] })
  ],
  base: '/',
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    chunkSizeWarningLimit: 1000,
    rollupOptions: {
      output: {
        entryFileNames: 'js/[name]-[hash].js',
        chunkFileNames: 'js/[name]-[hash].js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name?.endsWith('.css')) {
            return 'css/[name]-[hash][extname]'
          }
          return 'assets/[name]-[hash][extname]'
        },
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('@tiptap') || id.includes('prosemirror')) return 'vendor-editor'
            if (id.includes('vue') || id.includes('pinia') || id.includes('vue-router') || id.includes('vue-i18n')) return 'vendor-vue'
            if (id.includes('@microsoft/signalr')) return 'vendor-signalr'
            return 'vendor'
          }
        }
      }
    }
  },
  css: {
    lightningcss: {
      // Workaround: vue3-easy-data-table utilise des variables CSS sans préfixe "--"
      // À retirer quand ce package sera remplacé
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
