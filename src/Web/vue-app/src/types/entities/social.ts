export interface Member {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  profileImageUrl?: string
  avatarColor: string
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
  avatarColor?: string
  role: string
}

export interface Post {
  id: string
  groupId?: string
  authorMemberId: string
  authorName: string
  authorProfileImageUrl?: string
  authorAvatarColor?: string
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
  originalUrl?: string
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
  authorAvatarColor?: string
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
    avatarColor?: string
  }
  lastMessage?: {
    content: string
    senderName: string
    created: string
    isMine?: boolean
  }
  unreadCount: number
}

export interface MessageMedia {
  id: string
  mediaUrl: string
  thumbnailUrl?: string
  originalUrl?: string
  contentType: string
  size: number
  sortOrder: number
}

export interface JoinRequestInfo {
  id: string
  groupId: string
  groupName: string
  requesterMemberId: string
  requesterName: string
  status: 'Pending' | 'Accepted' | 'Rejected'
  resolvedByName?: string
}

export interface Message {
  id: string
  conversationId: string
  senderMemberId: string
  senderName: string
  content: string
  media?: MessageMedia[]
  created: string
  messageType?: string
  joinRequest?: JoinRequestInfo
}
