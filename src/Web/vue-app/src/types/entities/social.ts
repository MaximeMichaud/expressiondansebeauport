export interface Member {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  profileImageUrl?: string
  roles: string[]
}

export interface Group {
  id: string
  name: string
  description?: string
  imageUrl?: string
  inviteCode?: string
  season: string
  isArchived: boolean
  memberCount: number
}

export interface GroupMember {
  id: string
  memberId: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  profileImageUrl?: string
  role: string
}

export interface Post {
  id: string
  groupId?: string
  authorMemberId: string
  authorName: string
  authorProfileImageUrl?: string
  content: string
  type: string
  isPinned: boolean
  viewCount: number
  likeCount: number
  commentCount: number
  hasLiked: boolean
  media: PostMedia[]
  poll?: Poll
  created: string
}

export interface PostMedia {
  id: string
  mediaUrl: string
  thumbnailUrl?: string
  contentType: string
  size: number
  sortOrder: number
}

export interface Comment {
  id: string
  postId: string
  authorMemberId: string
  authorName: string
  authorProfileImageUrl?: string
  content: string
  created: string
}

export interface Poll {
  id: string
  question: string
  allowMultipleAnswers: boolean
  options: PollOption[]
  hasVoted: boolean
}

export interface PollOption {
  id: string
  text: string
  voteCount: number
  percentage: number
  hasVoted: boolean
}

export interface Conversation {
  id: string
  otherMember: {
    id: string
    fullName: string
    profileImageUrl?: string
  }
  lastMessage?: {
    content: string
    senderName: string
    created: string
  }
  unreadCount: number
}

export interface Message {
  id: string
  conversationId: string
  senderMemberId: string
  senderName: string
  content: string
  created: string
}
