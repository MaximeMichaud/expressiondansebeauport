import {defineStore} from 'pinia'
import {Member} from "@/types";
import {Role} from "@/types/enums";

interface MemberState {
  member: Member
}

export const useMemberStore = defineStore('member', {
  state: (): MemberState => ({
    member: {roles: [] as string[]} as Member
  }),

  actions: { // methods
    setMember(member: Member) {
      this.member = member
    },
    hasRole(role: Role): boolean {
      if (!this.member.roles)
        return false;
      return this.member.roles.some(x => x === role)
    },
    hasOneOfTheseRoles(roles: Role[]): boolean {
      return roles.some(x => this.member.roles && this.member.roles.includes(x));
    },
    reset() {
      this.member = {}
    }
  },

  getters: { // computed
    getMember: (state) => state.member,
  },

  persist: {
    storage: sessionStorage
  }
})