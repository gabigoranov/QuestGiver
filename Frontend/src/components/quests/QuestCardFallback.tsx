import { XCircle } from "lucide-react"

/**
 * A fallback in case of an error with the quest card
 *
 * @export
 * @return {*} 
 */
export default function QuestCardFallback() {
  return (
    <div className="bg-card rounded-2xl p-6 w-full max-w-sm shadow-glow-soft flex flex-col items-center justify-center gap-4 text-muted-foreground text-center">
        <XCircle size={28} className="w-12 h-12"/>

      <h2 className="font-heading uppercase font-semibold tracking-widest">Something went wrong while loading the quest.</h2>
      <p className="font-sans text-justify">Please try again later, or tune in for tomorrow's quest.</p>
    </div>
  )
}
