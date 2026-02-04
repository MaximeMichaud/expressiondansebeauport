import {defineStore} from 'pinia'
import {User} from "@/types";
import {Role} from "@/types/enums";

interface UserState {
  username: string
  user: User
}

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    username: '',
    user: {roles: [] as string[]} as User
  }),

  actions: { // methods
    setUsername(email: string) {
      this.username = email
    },
    setUser(user: User) {
      this.user = user
    },
    hasRole(role: Role): boolean {
      if (!this.user)
        return false;
      return this.user.roles.some(x => x === role)
    },
    hasOneOfTheseRoles(roles: Role[]): boolean {
      return roles.some(x => this.user.roles.includes(x));
    },
    reset() {
      this.username = '';
      this.user = {roles: [] as string[]} as User
    }
  },

  getters: { // computed
    getUser: (state) => state.user,
    getUsername: (state) => state.username,
  },

  persist: true
})