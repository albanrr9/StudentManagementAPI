namespace StudentManagementAPI.Models
{
    public class Grade
    {
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public string Subject { get; set; }
        public decimal GradeValue { get; set; }

    }
}