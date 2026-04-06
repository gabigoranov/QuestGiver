import type { TokenDTO } from "./TokenDTO";
import type { UserDTO } from "./UserDTO";

export type AuthResponse = {
    user: UserDTO,
    token: TokenDTO,
    hasInterestsInfo: boolean
};