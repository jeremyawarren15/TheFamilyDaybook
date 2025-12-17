# Product Requirements Document: The Family Daybook

## Overview
The Family Daybook is a Blazor web application designed to help homeschool parents track their children's educational progress. The application focuses on broad-stroke tracking, behavioral pattern recognition, and flexible metric tracking to identify learning barriers.

## Architecture Stack
- **Frontend**: Blazor (Server-side)
- **Styling**: Tailwind CSS
- **Backend**: C# / .NET 9
- **Dependency Injection**: Autofac
- **Testing**: NUnit with FakeItEasy for mocking
- **Data Access**: Entity Framework Core

---

## Phase 1: Foundation & MVP (Minimum Viable Product)

### 1.1 Authentication & User Management
- [x] Set up authentication system (ASP.NET Core Identity or similar)
- [x] User registration/login pages
- [x] User session management
- [x] Support multiple users (parents) per family
- [x] Basic user profile management

### 1.2 Database Setup
- [x] Configure Entity Framework with ApplicationDbContext
- [x] Set up database migrations
- [x] Configure database connection (SQLite for dev, PostgreSQL/SQL Server for production)
- [x] Seed initial data if needed

### 1.3 Student Management (Core)
- [x] Create Student model (Name, DateOfBirth, Notes, etc.)
- [x] Student CRUD operations (Create, Read, Update, Delete)
- [x] Student list page/view
- [x] Add student form
- [x] Edit student form
- [x] Support up to 12 students per family (implemented without limit - allows any number)
- [x] Associate students with parent/user accounts

### 1.4 Subject Management
- [ ] Create Subject model (Name, Description, etc.)
- [ ] Subject CRUD operations
- [ ] Subject list page/view
- [ ] Add subject form
- [ ] Edit subject form
- [ ] All subjects are custom (no predefined list)
- [ ] Subjects can be shared across all children in a family

### 1.5 Basic Daily Log (MVP)
- [ ] Create DailyLog model (Date, StudentId, Notes, etc.)
- [ ] Daily log CRUD operations
- [ ] Daily log entry page/form
- [ ] Ability to select student and date
- [ ] Notes field for free-form text entry
- [ ] Basic validation (one log per student per day)

### 1.6 Subject Tracking (MVP)
- [ ] Create StudentSubject junction/association model
- [ ] Associate subjects with students (many-to-many)
- [ ] Display subjects for each student
- [ ] Add/remove subjects from student
- [ ] Track which subjects were worked on in daily log
- [ ] Basic subject activity notes in daily log

### 1.7 Basic Navigation & Layout
- [ ] Main navigation menu
- [ ] Layout component with navigation
- [ ] Routing setup for all pages
- [ ] Basic responsive design with Tailwind

---

## Phase 2: Goals & Time Tracking

### 2.1 Weekly Goals System
- [ ] Create Goal model (Description, StudentId, SubjectId, WeekStartDate, IsCompleted, etc.)
- [ ] Goal CRUD operations
- [ ] Weekly goals page/view
- [ ] Set goals for the week (per student, per subject)
- [ ] Mark goals as complete/incomplete
- [ ] Display goals for current week
- [ ] Uncompleted goals can roll over to next week
- [ ] Weekly goal rollover prompt when new week starts
- [ ] Ability to modify goals at any time

### 2.2 Time Tracking
- [ ] Create TimeEntry model (StudentId, SubjectId, Date, StartTime, EndTime, Duration, etc.)
- [ ] Time entry CRUD operations
- [ ] Time entry form with clock widget
- [ ] Start time picker/input
- [ ] End time picker/input
- [ ] Calculate duration automatically
- [ ] Display time spent per subject per day
- [ ] Manual entry (no timer functionality needed)
- [ ] Time doesn't need to be exact/precise

### 2.3 Dashboard Enhancement
- [ ] Create dashboard/home page
- [ ] Display weekly goals on one side
- [ ] Display students on the other side
- [ ] Visual indicator for students missing today's daily log
- [ ] Quick access to create daily log
- [ ] Quick access to view/edit goals
- [ ] Week navigation/view

---

## Phase 3: Metrics & Behavioral Tracking

### 3.1 Custom Metrics System
- [ ] Create Metric model (Name, Description, IsFamilyWide, IsPerChild, etc.)
- [ ] Metric CRUD operations
- [ ] Metric configuration page
- [ ] Create custom metrics (e.g., "Ate Apples", "Noise Distractions")
- [ ] Configure metrics as family-wide or per-child
- [ ] Associate metrics with specific students (if per-child)
- [ ] Pre-defined metrics that make sense (optional, TBD)
- [ ] Metric types/categories (behavioral, environmental, etc.)

### 3.2 Daily Log with Metrics
- [ ] Extend DailyLog model to include metrics
- [ ] Create DailyLogMetric junction model
- [ ] Daily log form with tapable metric selection
- [ ] Display metrics as buttons/tappable elements (no typing)
- [ ] Select/deselect metrics for the day
- [ ] Visual feedback for selected metrics
- [ ] Notes section (only typing required)
- [ ] Save daily log with selected metrics

### 3.3 Metric Selection Interface
- [ ] Tapable metric buttons/widgets
- [ ] Group metrics logically (family-wide vs per-child)
- [ ] Visual indicators for selected metrics
- [ ] Quick metric selection workflow
- [ ] Mobile-friendly tap targets

---

## Phase 4: Reporting & Analytics

### 4.1 Reporting Page
- [ ] Create reporting/analytics page (not named "Reporting")
- [ ] Metric trend visualization
- [ ] Date range selection
- [ ] Student selection/filtering
- [ ] Metric selection/filtering
- [ ] Display trends over time
- [ ] Correlation analysis (basic)
- [ ] Export capabilities (optional)

### 4.2 Trend Analysis
- [ ] Identify patterns in metrics
- [ ] Visualize metric frequency over time
- [ ] Compare metrics across students
- [ ] Highlight potential correlations
- [ ] Help identify behavioral barriers to learning

### 4.3 Data Visualization
- [ ] Charts/graphs for trends (using a charting library)
- [ ] Time-based visualizations
- [ ] Metric comparison views
- [ ] Student progress indicators

---

## Phase 5: Polish & Enhancement

### 5.1 User Experience Improvements
- [ ] Improve mobile responsiveness
- [ ] Add loading states
- [ ] Add error handling and user feedback
- [ ] Improve form validation
- [ ] Add confirmation dialogs for destructive actions
- [ ] Improve navigation flow

### 5.2 Data Management
- [ ] Data export functionality
- [ ] Data backup/restore (optional)
- [ ] Archive old logs (optional)
- [ ] Bulk operations (optional)

### 5.3 Testing
- [ ] Unit tests for core business logic
- [ ] Integration tests for data access
- [ ] UI component tests
- [ ] End-to-end user flow tests

### 5.4 Performance & Optimization
- [ ] Database query optimization
- [ ] Caching where appropriate
- [ ] Lazy loading for large datasets
- [ ] Pagination for lists

---

## Technical Implementation Notes

### Data Models (Key Entities)
- **User/Parent**: Authentication and user management
- **Student**: Child information (Name, DateOfBirth, Notes, etc.)
- **Subject**: Custom subjects (Math, Science, Piano, Reading, etc.)
- **StudentSubject**: Many-to-many relationship between students and subjects
- **DailyLog**: Daily entry for a student (Date, Notes, Metrics)
- **Goal**: Weekly goals (Description, StudentId, SubjectId, WeekStartDate, IsCompleted)
- **TimeEntry**: Time tracking (StudentId, SubjectId, Date, StartTime, EndTime)
- **Metric**: Custom metrics (Name, IsFamilyWide, IsPerChild)
- **DailyLogMetric**: Junction table for daily log metrics

### Key Pages/Components
1. **Dashboard/Home**: Weekly goals + Students with daily log indicators
2. **Students**: Student management (list, add, edit)
3. **Subjects**: Subject management (list, add, edit)
4. **Daily Log**: Create/edit daily log with metrics
5. **Weekly Goals**: Set and manage weekly goals
6. **Time Tracking**: Log time spent on subjects
7. **Metrics Configuration**: Create and configure metrics
8. **Reporting/Analytics**: View trends and patterns

### MVP Definition
The minimum viable product should allow:
- User authentication and login
- Create and manage students
- Create and manage custom subjects
- Associate subjects with students
- Create daily logs with notes
- Track which subjects were worked on in daily logs

---

## Progress Tracking

**Current Status**: In Development
**Last Updated**: December 17, 2024

### Completion Summary
- Phase 1 (Foundation & MVP): 3/7 sections complete
- Phase 2 (Goals & Time Tracking): 0/3 sections complete
- Phase 3 (Metrics & Behavioral Tracking): 0/3 sections complete
- Phase 4 (Reporting & Analytics): 0/3 sections complete
- Phase 5 (Polish & Enhancement): 0/4 sections complete

---

## Notes & Future Considerations
- Consider adding predefined metrics that make sense for most families
- Explore data visualization libraries for reporting
- Consider mobile app version in the future
- May need to add family/group concept if multiple families use the same instance
- Consider adding notifications/reminders for daily logs

