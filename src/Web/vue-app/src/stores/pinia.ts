import { createPinia } from "pinia";
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

const p = createPinia()
p.use(piniaPluginPersistedstate)

export const pinia = p;