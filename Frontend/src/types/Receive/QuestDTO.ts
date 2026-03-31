export type QuestDTO = {
  id: string; 
  title: string; 
  description: string; 
  dateCreated: Date; 
  scheduledDate: Date; 
  dateCompleted?: Date | null; 
  isCompleted: boolean; 
  rewardPoints: number;
  userId: string; 
};