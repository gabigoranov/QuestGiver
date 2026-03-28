import { Button } from '@/components/ui/button';
import { Sparkles, Target, Zap } from 'lucide-react';

export function Hero() {
  return (
    <section className="relative overflow-hidden py-20 sm:py-32">
      {/* Background gradient */}
      <div className="absolute inset-0 -z-10">
        <div className="absolute left-1/2 top-0 -translate-x-1/2 -translate-y-1/2 h-[500px] w-[500px] rounded-full bg-primary/20 blur-[100px]" />
        <div className="absolute right-0 bottom-0 translate-x-1/3 translate-y-1/3 h-[400px] w-[400px] rounded-full bg-accent/20 blur-[100px]" />
      </div>

      <div className="container mx-auto px-4">
        <div className="text-center">
          <div className="inline-flex items-center rounded-full border border-primary/20 bg-primary/5 px-4 py-1.5 text-sm font-medium text-primary mb-6">
            <Sparkles className="mr-2 h-4 w-4" />
            Your journey begins here
          </div>
          
          <h1 className="text-4xl font-bold tracking-tight sm:text-6xl lg:text-7xl mb-6">
            Create Epic{' '}
            <span className="bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
              Quests
            </span>{' '}
            for Your Community
          </h1>
          
          <p className="mx-auto max-w-2xl text-lg text-muted-foreground mb-8">
            QuestGiver helps you build engaging experiences, gamify your projects, 
            and motivate your community with customizable quests and rewards.
          </p>
          
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
            <Button size="lg" className="w-full sm:w-auto text-base px-8">
              <Zap className="mr-2 h-5 w-5" />
              Start Creating
            </Button>
            <Button variant="outline" size="lg" className="w-full sm:w-auto text-base px-8">
              <Target className="mr-2 h-5 w-5" />
              View Examples
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
}
