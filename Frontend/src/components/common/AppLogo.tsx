import { LucideCompass } from 'lucide-react'

export default function AppLogo() {
  return (
    <div className='flex flex-row gap-2 items-center'>
        <LucideCompass size={32} className='text-primary' />
        <span className='font-heading text-2xl font-bold text-primary'>QuestBound</span>
    </div>
  )
}
