import {defineStore} from 'pinia'
import {Administrator} from "@/types";

interface AdministratorState {
  administrator: Administrator
}

export const useAdministratorStore = defineStore('administrator', {
  state: (): AdministratorState => ({
    administrator: {} as Administrator
  }),

  actions: { // methods
    setAdministrator(administrator: Administrator) {
      this.administrator = administrator
    },
    reset() {
      this.administrator = {}
    }
  },

  getters: { // computed
    getAdministrator: (state) => state.administrator,
  },

  persist: {
    storage: sessionStorage
  }
})