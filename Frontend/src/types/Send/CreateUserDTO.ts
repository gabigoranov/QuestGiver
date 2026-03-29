export type CreateUserDTO = {
  username: string,
  birthDate: Date,
  description: string,
  email: string,
  password: string,
  avatarUrl?: string,
};