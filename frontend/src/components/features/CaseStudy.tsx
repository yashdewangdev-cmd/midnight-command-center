import './CaseStudy.css';

interface CaseStudyProps {
  title: string;
  subtitle: string;
  description: string;
  techStack: string[];
  highlights: string[];
  imageUrl?: string;
  repositoryUrl?: string;
  liveUrl?: string;
  id?: string;
}

/**
 * Reusable case study card for portfolio project showcase.
 */
export function CaseStudy({
  title,
  subtitle,
  description,
  techStack,
  highlights,
  repositoryUrl,
  liveUrl,
  id,
}: CaseStudyProps) {
  return (
    <article id={id} className="case-study glass-card animate-slide-up">
      <div className="case-study__content">
        <div className="case-study__header">
          <h3 className="case-study__title">{title}</h3>
          <p className="case-study__subtitle">{subtitle}</p>
        </div>

        <p className="case-study__description">{description}</p>

        <div className="case-study__tech-stack">
          {techStack.map((tech) => (
            <span key={tech} className="case-study__tech-tag">
              {tech}
            </span>
          ))}
        </div>

        <ul className="case-study__highlights">
          {highlights.map((h, i) => (
            <li key={i} className="case-study__highlight">
              <span className="case-study__highlight-icon">◆</span>
              {h}
            </li>
          ))}
        </ul>

        <div className="case-study__links">
          {repositoryUrl && (
            <a href={repositoryUrl} target="_blank" rel="noopener noreferrer" className="btn btn-secondary btn-sm">
              🔗 Repository
            </a>
          )}
          {liveUrl && (
            <a href={liveUrl} target="_blank" rel="noopener noreferrer" className="btn btn-primary btn-sm">
              🌐 Live Demo
            </a>
          )}
        </div>
      </div>
    </article>
  );
}
