# Thuật toán và cơ chế chấm điểm (First Fit + Tabu Search)

Tài liệu này mô tả trạng thái đã triển khai trong code, bao gồm hard constraints theo RoomType/DeliveryMode.

## 1. HardSoftScore

- Định nghĩa: `Domain/Common/HardSoftScore.cs`.
- Gồm HardScore và SoftScore (int).
- So sánh: HardScore ưu tiên trước; bằng nhau thì so SoftScore.

## 2. ScheduleScorer (`Application/Services/ScheduleScorer.cs`)

### 2.1 Hard constraints — RoomType + DeliveryMode (O(1))

- Room index bằng dictionary `roomById`.
- Mỗi lesson:
  - Online + có RoomId → hard −1.
  - Offline + không có RoomId → hard −1.
  - Offline + RoomId không tồn tại hoặc RoomType khác RequiredRoomType → hard −1.

### 2.2 Hard constraints — xung đột lịch

- roomConflict: cùng RoomId + TimeSlotId → phạt n*(n−1)/2.
- teacherConflict: cùng TeacherId + TimeSlotId.
- studentGroupConflict: cùng StudentGroupId + TimeSlotId.

### 2.3 Soft constraints

- teacherRoomStability: cùng GV dạy nhiều phòng → phạt.
- teacherTimeEfficiency: 2 tiết liền kề cùng ngày → thưởng.
- studentGroupSubjectVariety: cùng nhóm học cùng môn liền kề → phạt.

## 3. FirstFitSolver (`Application/Services/FirstFitSolver.cs`)

- Clone schedule, sắp rooms theo Id, timeslots theo ngày/giờ.
- Duyệt lesson: giữ nếu hợp lệ, thử cặp (room, timeslot) không conflict, fallback phần tử đầu.
- Chỉ tránh conflict trùng lịch, chưa tối ưu DeliveryMode/RoomType.

## 4. TabuSearchSolver (`Application/Services/TabuSearchSolver.cs`)

- Queue + HashSet làm Tabu List.
- Mỗi vòng: sinh neighborhood (Change/Swap via ScheduleMove), chọn best non-tabu.
- Aspiration: chấp nhận move tabu nếu vượt global best.
- Cấu hình: TabuTenure=50, MaxIterations=5000, NeighborhoodSize=300.

## 5. ScheduleOrchestrator (`Application/Services/ScheduleOrchestrator.cs`)

- Chạy FirstFitSolver → TabuSearchSolver → trả schedule + HardSoftScore.

## 6. Enum parse fail-fast (`SortSchedule/Extensions/EnumParser.cs`)

- RoomType: Theory, Practice. DeliveryMode: Offline, Online.
- Normalize(FormC) → Trim() → Enum.Parse(ignoreCase: true).
- Invalid → exception ngay, không fallback.

## 7. Test coverage

| File | Mô tả |
|------|-------|
| ScheduleScorerTests.cs | Hard conflict + soft reward |
| Scoring/ConstraintTests.cs | RoomType/DeliveryMode constraints |
| FirstFitSolverTests.cs | Gán lịch giảm xung đột |
| TabuSearchSolverTests.cs | Nghiệm không tệ hơn ban đầu |
| TestScheduleFactory.cs | Factory dữ liệu test |
| AuthServiceTests.cs | Register/Login/Refresh flows |
| TokenServiceTests.cs | JWT + refresh token hash |
| EnumParserTests.cs | Trim, case, UTF-8, fail-fast |
