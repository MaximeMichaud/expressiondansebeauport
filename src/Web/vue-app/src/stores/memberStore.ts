import { defineStore } from 'pinia'
import type { Member } from '@/types/entities'

interface MemberState {
  member: Member
  unreadMessageCount: number
}

export const useMemberStore = defineStore('member', {
  state: (): MemberState => ({
    member: {} as Member,
    unreadMessageCount: 0
  }),
  actions: {
    setMember(member: Member) { this.member = member },
    setUnreadCount(count: number) { this.unreadMessageCount = count },
    incrementUnreadCount() { this.unreadMessageCount++ },
    decrementUnreadCount() { if (this.unreadMessageCount > 0) this.unreadMessageCount-- },
    reset() { this.member = {} as Member; this.unreadMessageCount = 0 }
  },
  getters: {
    getMember: (state) => state.member,
    isLoggedIn: (state) => !!state.member.id,
  }
})
