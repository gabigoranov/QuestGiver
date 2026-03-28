import { Compass, Gift, Users, Trophy, Clock, Shield } from 'lucide-react';

const features = [
  {
    icon: Compass,
    title: 'Quest Creation',
    description: 'Design custom quests with multiple objectives, rewards, and difficulty levels.',
  },
  {
    icon: Gift,
    title: 'Reward System',
    description: 'Motivate users with points, badges, and customizable rewards.',
  },
  {
    icon: Users,
    title: 'Community Driven',
    description: 'Build engaged communities with collaborative challenges and leaderboards.',
  },
  {
    icon: Trophy,
    title: 'Achievements',
    description: 'Track progress and celebrate milestones with beautiful achievement cards.',
  },
  {
    icon: Clock,
    title: 'Time-Limited Events',
    description: 'Create urgency with seasonal events and limited-time challenges.',
  },
  {
    icon: Shield,
    title: 'Secure & Reliable',
    description: 'Enterprise-grade security to protect your community data.',
  },
];

export function Features() {
  return (
    <section className="py-20 sm:py-32 bg-muted/30">
      <div className="container mx-auto px-4">
        <div className="text-center mb-16">
          <h2 className="text-3xl font-bold sm:text-4xl mb-4">
            Everything You Need to{' '}
            <span className="bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
              Engage
            </span>
          </h2>
          <p className="text-muted-foreground text-lg max-w-2xl mx-auto">
            Powerful features to help you create memorable experiences for your community.
          </p>
        </div>

        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((feature) => (
            <div
              key={feature.title}
              className="group relative rounded-2xl border border-border bg-card p-6 transition-all hover:shadow-lg hover:shadow-primary/5"
            >
              <div className="inline-flex rounded-xl bg-primary/10 p-3 mb-4 group-hover:bg-primary/20 transition-colors">
                <feature.icon className="h-6 w-6 text-primary" />
              </div>
              <h3 className="text-lg font-semibold mb-2">{feature.title}</h3>
              <p className="text-muted-foreground">{feature.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
