import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import svgLoader from 'vite-svg-loader'
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [
    vue(),
    svgLoader(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      strategies: 'injectManifest',
      srcDir: 'src',
      filename: 'sw.ts',
      injectManifest: {
        globPatterns: ['**/*.{js,css,html,svg,png,woff2}']
      },
      manifest: {
        name: 'Expression Danse Beauport',
        short_name: 'EDB',
        description: 'Portail communautaire d\'Expression Danse Beauport',
        theme_color: '#be1e2c',
        background_color: '#ffffff',
        display: 'standalone',
        orientation: 'portrait',
        start_url: '/social',
        scope: '/',
        icons: [
          { src: '/icons/192.png', sizes: '192x192', type: 'image/png' },
          { src: '/icons/512.png', sizes: '512x512', type: 'image/png' }
          // No 'maskable' variant: our icons are pre-rounded with
          // transparent corners, which conflicts with Android's mask.
        ]
      },
      devOptions: { enabled: true, type: 'module', navigateFallback: 'index.html' }
    })
  ],
  base: '/',
  build: {
    outDir: '../wwwroot',
    // emptyOutDir must stay FALSE: wwwroot/uploads/ contains user-uploaded
    // files that would be wiped by a clean build, causing data loss.
    emptyOutDir: false,
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
          const normalizedId = id.replaceAll('\\', '/')
          if (!normalizedId.includes('/node_modules/')) return undefined
          // Tiptap/ProseMirror exclus intentionnellement : laisser Rollup les grouper
          // naturellement avec les chunks async (RichTextBlock, AdminPageEditor).
          // Un manualChunk explicite force un modulepreload dans index.html même
          // quand le code est derrière un defineAsyncComponent.
          if (normalizedId.includes('/node_modules/@tiptap/') || normalizedId.includes('/node_modules/prosemirror')) return undefined
          if (
            normalizedId.includes('/node_modules/vue/') ||
            normalizedId.includes('/node_modules/@vue/') ||
            normalizedId.includes('/node_modules/pinia/') ||
            normalizedId.includes('/node_modules/vue-router/') ||
            normalizedId.includes('/node_modules/vue-i18n/')
          ) return 'vendor-vue'
          if (normalizedId.includes('/node_modules/@microsoft/signalr/')) return 'vendor-signalr'
          return undefined
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
    allowedHosts: ['.trycloudflare.com', '.ngrok.io', '.ngrok-free.app', 'localhost'],
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
