import {defineStore} from 'pinia'
import {IPerson} from "@/types";

interface PersonState {
  person: IPerson
}

export const usePersonStore = defineStore('person', {
  state: (): PersonState => ({
    person: {} as IPerson
  }),

  actions: { // methods
    setPerson(person: IPerson) {
      this.person = person
    },
    reset() {
      this.person = {} as IPerson
    }
  },

  getters: { // computed
    getPerson: (state) => state.person,
  },

  persist: true
})