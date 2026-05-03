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
          { src: '/icons/512.png', sizes: '512x512', type: 'image/png' },
          { src: '/icons/512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' }
        ]
      },
      devOptions: { enabled: false }
    })
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
