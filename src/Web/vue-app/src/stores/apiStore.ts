import { defineStore } from 'pinia'

interface ApiState {
    needToLogout: boolean
}

export const useApiStore = defineStore('api', {
    state: (): ApiState => ({
        needToLogout: false
    }),

    actions: { // methods
        setNeedToLogout (needToLogout:boolean) {
            this.needToLogout = needToLogout
        }
    },

    getters: { // computed
        getNeedToLogout: (state) => state.needToLogout
    }
})