export type UserDTO = {
    id: string,
    username: string,
    birthDate: Date,
    description: string,
    email: string,
    avatarUrl?: string,
    level: number,
    experiencePoints: number,
    nextLevelExperience: number
}