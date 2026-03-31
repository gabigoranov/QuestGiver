import type { QuestStatusType } from "./QuestStatusType"

export type GroupDTO = {
    id: string,
    title: string,
    description: string,
    dateCreated: Date,
    membersCount: number,
    currentQuestStatus: QuestStatusType
}