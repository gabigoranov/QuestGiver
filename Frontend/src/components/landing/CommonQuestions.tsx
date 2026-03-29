import { ChevronDown } from "lucide-react";
import { useState, useRef, useEffect } from "react";

interface FAQItem {
  question: string;
  answer: string;
}

const faqs: FAQItem[] = [
  {
    question: "How are quests generated?",
    answer:
      "Quests are generated using AI that analyzes your goals, preferences, and past activity. Each quest is uniquely tailored to push you just the right amount — challenging, but always achievable.",
  },
  {
    question: "Can I skip a quest?",
    answer:
      "Yes! You can skip a quest once per day without any penalty. Skipping more than once will cost you a small number of points, so choose wisely and save your skips for when you truly need them.",
  },
  {
    question: "How do I earn points?",
    answer:
      "Points are earned by completing quests, maintaining daily streaks, and hitting milestone goals. Bonus points are awarded for completing quests early or achieving them with extra precision.",
  },
];


/**
 * Used in the CommonQuestions component
 *
 * @param {{
 *   item: FAQItem;
 *   index: number;
 *   isOpen: boolean;
 *   onToggle: () => void;
 * }} {
 *   item,
 *   isOpen,
 *   onToggle,
 * }
 * @return {*} 
 */
function AccordionItem({
  item,
  isOpen,
  onToggle,
}: {
  item: FAQItem;
  index: number;
  isOpen: boolean;
  onToggle: () => void;
}) {
  // -- Height animation via scrollHeight ref --
  const contentRef = useRef<HTMLDivElement>(null);
  const [height, setHeight] = useState(0);

  useEffect(() => {
    if (contentRef.current) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setHeight(isOpen ? contentRef.current.scrollHeight : 0);
    }
  }, [isOpen]);

  return (
    // Card wrapper — .card applies glass-bg + backdrop-blur + glow-soft from index.css
    // border-color and shadow switch on open state using palette tokens
    <div
      className={[
        "overflow-hidden rounded-xl border",
        "transition-[border-color] duration-300",
        isOpen
          ? "border-primary/30"
          : "border-border",
      ].join(" ")}
    >
      {/* ── Trigger Button ── */}
      <button
        onClick={onToggle}
        className="flex w-full items-center justify-between gap-3 px-5.5 py-5 text-left"
      >
        {/* Question label — font-heading maps to Space Grotesk via @theme */}
        <span className="font-heading text-[0.975rem] font-semibold tracking-tight text-foreground">
          {item.question}
        </span>

        {/* ── Chevron bubble ── */}
        {/* bg-primary / bg-muted from @theme inline surface mapping */}
        <span
          className={[
            "flex h-7 w-7 shrink-0 items-center justify-center rounded-full",
            "transition-[background-color] duration-300",
            isOpen ? "bg-primary" : "bg-muted",
          ].join(" ")}
          style={{
            // Rotation uses spring easing — not a Tailwind default, kept inline
            transform: isOpen ? "rotate(180deg)" : "rotate(0deg)",
            transition:
              "background-color 0.3s ease, transform 0.35s cubic-bezier(0.34, 1.56, 0.64, 1)",
          }}
        >
          <ChevronDown size={16} className={isOpen ? "text-(--on-primary)" : "text-muted-foreground"} />
        </span>
      </button>

      {/* ── Animated Content Panel ── */}
      {/* Height is driven by JS scrollHeight for pixel-perfect expansion */}
      <div
        className="overflow-hidden transition-[height] duration-400 ease-in-out"
        style={{ height: `${height}px` }}
      >
        <div ref={contentRef} className="px-5.5 pb-5">

          {/* Gradient divider — fades in after panel opens, uses primary token */}
          <div
            className={[
              "mb-3.5 h-px bg-linear-to-r from-primary/30 to-transparent",
              "transition-opacity delay-100 duration-300",
              isOpen ? "opacity-100" : "opacity-0",
            ].join(" ")}
          />

          {/* Answer text — font-sans maps to Manrope via @theme */}
          {/* Slides up + fades in with a slight delay after panel expands */}
          <p
            className={[
              "font-sans text-sm leading-relaxed text-muted-foreground",
              "transition-[opacity,transform] delay-100 duration-300",
              isOpen ? "translate-y-0 opacity-100" : "-translate-y-1.5 opacity-0",
            ].join(" ")}
          >
            {item.answer}
          </p>

        </div>
      </div>
    </div>
  );
}




/**
 * Part of the landing page
 *
 * @export
 * @return {*} 
 */
export default function CommonQuestions() {
  const [openIndex, setOpenIndex] = useState<number | null>(null);

  const toggle = (i: number) => setOpenIndex(openIndex === i ? null : i);

  return (
    // bg-background resolves to --surface (#121212) via @theme inline
    <section id="faq" className="flex h-auto items-center justify-center bg-background px-4 py-32">
      <div className="w-full max-w-106.25">

        {/* ── Section Heading ── */}
        {/* text-foreground → --on-surface via @theme */}
        <h2 className="mb-7 text-center font-heading text-[1.75rem] font-bold tracking-tight text-foreground">
          Common Questions
        </h2>

        {/* ── FAQ Accordion List ── */}
        <div className="flex flex-col gap-3">
          {faqs.map((item, i) => (
            <AccordionItem
              key={i}
              item={item}
              index={i}
              isOpen={openIndex === i}
              onToggle={() => toggle(i)}
            />
          ))}
        </div>

      </div>

      {/*
        animate-fade-slide-in is a custom keyframe.
        To move this out of the component, add to your globals.css:

        @keyframes fadeSlideIn {
          from { opacity: 0; transform: translateY(10px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-slide-in {
          animation: fadeSlideIn 0.4s ease both;
        }
      */}
      <style>{`
        @keyframes fadeSlideIn {
          from { opacity: 0; transform: translateY(10px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-slide-in {
          animation: fadeSlideIn 0.4s ease both;
        }
      `}</style>
    </section>
  );
}