import { CaseStudy } from '../components/features/CaseStudy';
import './Portfolio.css';

/**
 * Public-facing portfolio page with project case studies.
 */
export function Portfolio() {
  return (
    <div className="portfolio">
      <header className="portfolio__header animate-fade-in">
        <h1 className="portfolio__title">Portfolio</h1>
        <p className="portfolio__subtitle">
          Detailed case studies of key engineering projects
        </p>
      </header>

      <div className="portfolio__grid animate-slide-up">
        <CaseStudy
          id="case-study-mworktime"
          title="MWorkTime"
          subtitle="React + .NET MAUI Cross-Platform Integration"
          description="A comprehensive work time tracking application that bridges web and native mobile experiences. Built with React for the web dashboard and .NET MAUI for native iOS/Android clients, sharing a common .NET backend API. Features real-time synchronization, offline-first architecture, and multi-session cumulative time tracking with midnight-spanning calculation logic."
          techStack={['React 18', '.NET MAUI', '.NET 8 API', 'PostgreSQL', 'SignalR', 'TypeScript']}
          highlights={[
            'Cross-platform time tracking with sub-second synchronization between web and native clients',
            'Offline-first mobile architecture with conflict resolution and queue-based sync',
            'Multi-session cumulative calculation engine handling disconnected tracking periods',
            'Real-time dashboard with live timers, project breakdowns, and exportable PDF reports',
            'Clean Architecture backend with CQRS pattern and EF Core async data pipeline',
          ]}
          repositoryUrl="https://github.com/example/mworktime"
          liveUrl="https://mworktime.example.com"
        />

        <CaseStudy
          id="case-study-dcma"
          title="DCMA"
          subtitle="Kafka MQ · Docker Containers · High-Throughput Processing"
          description="A high-performance distributed content management and analytics platform engineered for enterprise-scale document processing. Leverages Apache Kafka for asynchronous message queuing, Docker containerization for microservice isolation, and a horizontally scalable processing pipeline capable of handling thousands of documents per minute with guaranteed delivery and exactly-once semantics."
          techStack={['Apache Kafka', 'Docker', '.NET 8', 'Redis', 'Elasticsearch', 'Kubernetes']}
          highlights={[
            'Event-driven microservices architecture with Apache Kafka as the central message backbone',
            'Containerized deployment via Docker with Kubernetes orchestration for auto-scaling',
            'High-throughput processing pipeline: 5,000+ documents/minute with exactly-once delivery guarantees',
            'Full-text search and analytics powered by Elasticsearch with real-time indexing',
            'Circuit breaker patterns and dead-letter queues for fault tolerance and graceful degradation',
          ]}
          repositoryUrl="https://github.com/example/dcma"
        />
      </div>
    </div>
  );
}
